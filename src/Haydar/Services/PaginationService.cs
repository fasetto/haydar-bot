using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Haydar.Services
{
    public class PaginationService
    {
        private readonly Dictionary<ulong, PaginatedMessage> _messages;
        private readonly DiscordSocketClient _client;

        public PaginationService(DiscordSocketClient client)
        {
            _messages = new Dictionary<ulong, PaginatedMessage>();
            _client = client;
            _client.ReactionAdded += OnReactionAdded;
        }

        /// <summary>
        /// Sends a paginated message (with reaction buttons)
        /// </summary>
        /// <param name="channel">The channel this message should be sent to</param>
        /// <param name="paginated">A <see cref="PaginatedMessage">PaginatedMessage</see> containing the pages.</param>
        /// <exception cref="Net.HttpException">Thrown if the bot user cannot send a message or add reactions.</exception>
        /// <returns>The paginated message.</returns>
        public async Task<IUserMessage> SendPaginatedMessageAsync(IMessageChannel channel, PaginatedMessage paginated)
        {
            var message = await channel.SendMessageAsync("", embed: paginated.GetEmbed());

            await message.AddReactionAsync(paginated.Options.EmoteFirst);
            await message.AddReactionAsync(paginated.Options.EmoteBack);
            await message.AddReactionAsync(paginated.Options.EmoteStop);
            await message.AddReactionAsync(paginated.Options.EmoteNext);
            await message.AddReactionAsync(paginated.Options.EmoteLast);

            _messages.Add(message.Id, paginated);

            if (paginated.Options.Timeout != TimeSpan.Zero)
            {
                var _ = Task.Delay(paginated.Options.Timeout).ContinueWith(async _t =>
                {
                    if (!_messages.ContainsKey(message.Id)) return;
                    if (paginated.Options.TimeoutAction == StopAction.DeleteMessage)
                        await message.DeleteAsync();
                    else if (paginated.Options.TimeoutAction == StopAction.ClearReactions)
                        await message.RemoveAllReactionsAsync();
                    _messages.Remove(message.Id);
                });
            }

            return message;
        }

        internal async Task OnReactionAdded(Cacheable<IUserMessage, ulong> messageParam, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await messageParam.GetOrDownloadAsync();

            if (message == null)
                return;

            if (!reaction.User.IsSpecified)
                return;

            if (_messages.TryGetValue(message.Id, out PaginatedMessage page))
            {
                if (reaction.UserId == _client.CurrentUser.Id) return;
                if (page.User != null && reaction.UserId != page.User.Id)
                {
                    var _ = message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                    return;
                }

                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

                if (reaction.Emote.Name == page.Options.EmoteFirst.Name)
                {
                    if (page.CurrentPage != 1)
                    {
                        page.CurrentPage = 1;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                    }
                }
                else if (reaction.Emote.Name == page.Options.EmoteBack.Name)
                {
                    if (page.CurrentPage != 1)
                    {
                        page.CurrentPage--;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                    }
                }
                else if (reaction.Emote.Name == page.Options.EmoteNext.Name)
                {
                    if (page.CurrentPage != page.Count)
                    {
                        page.CurrentPage++;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                    }
                }
                else if (reaction.Emote.Name == page.Options.EmoteLast.Name)
                {
                    if (page.CurrentPage != page.Count)
                    {
                        page.CurrentPage = page.Count;
                        await message.ModifyAsync(x => x.Embed = page.GetEmbed());
                    }
                }
                else if (reaction.Emote.Name == page.Options.EmoteStop.Name)
                {
                    if (page.Options.EmoteStopAction == StopAction.DeleteMessage)
                        await message.DeleteAsync();
                    else if (page.Options.EmoteStopAction == StopAction.ClearReactions)
                        await message.RemoveAllReactionsAsync();
                    _messages.Remove(message.Id);
                }
            }
        }
    }

    public class PaginatedMessage
    {
        public PaginatedMessage(IEnumerable<Page> pages, string title, string url, Color? embedColor = null, IUser user = null, AppearanceOptions options = null)
        {
            var embeds = new List<Embed>();
            int i = 1;
            foreach (var page in pages)
            {
                var builder = new EmbedBuilder()
                    .WithColor(embedColor ?? Color.Default)
                    .WithTitle(title)
                    .WithUrl(url)
                    .WithDescription(page?.Description ?? "")
                    .WithImageUrl(page?.ImageUrl ?? "")
                    .WithThumbnailUrl(page?.ThumbnailUrl ?? "")
                    .WithFooter(footer =>
                    {
                        footer.Text = $"Page {i++} of {pages.Count()}";
                    });

                if (page.Fields != null)
                    builder.Fields = page.Fields.ToList();

                embeds.Add(builder.Build());
            }
            Pages = embeds;
            Title = title;
            EmbedColor = embedColor ?? Color.Default;
            User = user;
            Options = options ?? new AppearanceOptions();
            CurrentPage = 1;
        }

        internal Embed GetEmbed()
        {
            return Pages.ElementAtOrDefault(CurrentPage - 1);
        }

        internal string Title { get; }
        internal Color EmbedColor { get; }
        internal IReadOnlyCollection<Embed> Pages { get; }
        internal IUser User { get; }
        internal AppearanceOptions Options { get; }
        internal int CurrentPage { get; set; }
        internal int Count => Pages.Count;
    }

    public class AppearanceOptions
    {
        public const string FIRST = "⏮";
        public const string BACK = "◀";
        public const string NEXT = "▶";
        public const string LAST = "⏭";
        public const string STOP = "⏹";

        public IEmote EmoteFirst { get; set; } = new Emoji(FIRST);
        public IEmote EmoteBack { get; set; } = new Emoji(BACK);
        public IEmote EmoteNext { get; set; } = new Emoji(NEXT);
        public IEmote EmoteLast { get; set; } = new Emoji(LAST);
        public IEmote EmoteStop { get; set; } = new Emoji(STOP);
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
        public StopAction EmoteStopAction { get; set; } = StopAction.ClearReactions;
        public StopAction TimeoutAction { get; set; } = StopAction.ClearReactions;
    }

    public enum StopAction
    {
        ClearReactions,
        DeleteMessage
    }

    public class Page
    {
        public IReadOnlyCollection<EmbedFieldBuilder> Fields { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public static class PaginationExtensions
    {
        [Obsolete("Addon builders on a client are discouraged, consider IServiceCollection.AddPaginator()")]
        public static DiscordSocketClient UsePaginator(this DiscordSocketClient client, IServiceCollection collection)
        {
            collection.AddSingleton(new PaginationService(client));
            return client;
        }
        /// <summary>
        /// Adds a PaginationService to a ServiceCollection
        /// </summary>
        /// <param name="collection">The service collection</param>
        /// <param name="client">The client this paginator will use</param>
        /// <param name="logger">A logging delegate</param>
        /// <returns>The service collection, with the pagiantor appended to it (for fluent patterns)</returns>
        public static IServiceCollection AddPaginator(this IServiceCollection collection, DiscordSocketClient client)
        {
            collection.AddSingleton(new PaginationService(client));
            return collection;
        }
        /// <summary>
        /// Adds a PaginationService to a ServiceCollection, assuming a DiscordSocketClient is already present in the collection, and that no logging method is wanted.
        /// </summary>
        /// <param name="collection">The service collection.</param>
        /// <returns>The service collection, with the pagiantor appended to it (for fluent patterns)</returns>
        public static IServiceCollection AddPaginator(this IServiceCollection collection)
        {
            collection.AddSingleton<PaginationService>();
            return collection;
        }
    }
}
