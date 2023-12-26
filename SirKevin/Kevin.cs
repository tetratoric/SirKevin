using Azure;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace KevinTheCrisp
{
    internal class Kevin
    {
        public static Task Main(string[] args) => new Kevin().MainAsync();

        
        private DiscordSocketClient _client;
        bool sentInitialMessage = false;
        SocketUser? lastAuthor;

        private GPTHandler _handler;
        int tokens = 0, interjectionChance = 25;

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };

            _client = new DiscordSocketClient(config);

            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.Log += Log;
            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;

            string token = "TOKEN";

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _handler = new GPTHandler();

            await Task.Delay(-1);
        }

        public async Task Client_Ready()
        {
            var kevinCommands = new SlashCommandBuilder()
                .WithName("kevin")
                .WithDescription("Manipulate Kevin to your will.")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("lore")
                    .WithDescription("Manipulates Kevin's lore, with which he uses to create his identity and responses.")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("set")
                        .WithDescription("Sets Kevin's lore to [value] and resets chat history.")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("value", ApplicationCommandOptionType.String, "Kevin's new lore", isRequired: true)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("get")
                        .WithDescription("Gets Kevin's current lore message.")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("probability")
                    .WithDescription("Sets the chance of Kevin randomly deciding to respond to any given message")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("value", ApplicationCommandOptionType.Integer, "1 in [value] chance of responding to a message", isRequired: true)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("defaultprompt")
                    .WithDescription("Manipulates Kevin's default prompt, which he uses to send his initial message at start up.")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("set")
                        .WithDescription("Sets Kevin's default prompt to [value] and resets chat history.")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("value", ApplicationCommandOptionType.String, "Kevin's new default prompt", isRequired: true)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("get")
                        .WithDescription("Gets Kevin's current default prompt.")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("restore")
                    .WithDescription("Restores Kevin to crisp, delicious, default settings.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("clear")
                    .WithDescription("Clears entire chat history, but keeps current settings.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                );
            try
            {
                await _client.CreateGlobalApplicationCommandAsync(kevinCommands.Build());
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                
                Console.WriteLine(json);
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "kevin":
                    await HandleKevinCommand(command);
                    break;
            }
        }

        public async Task HandleKevinCommand(SocketSlashCommand command)
        {
            var action = command.Data.Options.First();
            string actionName = action.Name;

            switch (actionName)
            {
                case "lore":
                    var loreOption = action.Options.First();
                    string loreActionName = loreOption.Name;
                    switch (loreActionName)
                    {
                        case "set":
                            string newLore = (string)loreOption.Options.First().Value;
                            _handler.lore = newLore;
                            Console.WriteLine("Replaced lore with: " + _handler.lore);
                            sentInitialMessage = false;

                            var newLoreResponse = new EmbedBuilder()
                                .WithAuthor(command.User)
                                .WithTitle("New Lore")
                                .WithDescription(_handler.lore)
                                .WithColor(Color.Gold);

                            await command.RespondAsync(embed: newLoreResponse.Build());
                            break;
                        case "get":
                            var getLoreResponse = new EmbedBuilder()
                                .WithTitle("Lore")
                                .WithDescription(_handler.lore)
                                .WithColor(Color.Gold);

                            await command.RespondAsync(embed: getLoreResponse.Build());
                            break;
                    }
                    break;

                case "probability":
                    int randNum = Convert.ToInt32(action.Options.First().Value);
                    string response;

                    if (randNum > 0) { 
                        interjectionChance = randNum;
                        response = $"Set Kevin's chance of randomly interjecting to 1 in {interjectionChance}.";
                    } else
                    {
                        response = $"{randNum} is an invalid value. It must be an integer greater than 0.";
                    }

                    var interjectionResponse = new EmbedBuilder()
                        .WithDescription(response)
                        .WithColor(Color.Gold);

                    await command.RespondAsync(embed: interjectionResponse.Build());
                    break;

                case "defaultprompt":
                    var promptOption = action.Options.First();
                    string promptOptionName = promptOption.Name;
                    switch (promptOptionName)
                    {
                        case "set":
                            string newPrompt = (string)promptOption.Options.First().Value;
                            _handler.defaultMessage = newPrompt;
                            Console.WriteLine("Replaced default prompt with: " + _handler.defaultMessage);
                            sentInitialMessage = false;

                            var newPromptResponse = new EmbedBuilder()
                                .WithAuthor(command.User)
                                .WithTitle("New Prompt")
                                .WithDescription(_handler.defaultMessage)
                                .WithColor(Color.Gold);

                            await command.RespondAsync(embed: newPromptResponse.Build());
                            break;

                        case "get":
                            var getPromptResponse = new EmbedBuilder()
                                .WithTitle("Prompt")
                                .WithDescription(_handler.defaultMessage)
                                .WithColor(Color.Gold);

                            await command.RespondAsync(embed: getPromptResponse.Build());
                            break;
                    }
                    break;

                case "restore":
                    _handler.RestoreKevin();
                    sentInitialMessage = false;

                    var restoreResponse = new EmbedBuilder()
                        .WithDescription("Restored Kevin to defaults.")
                        .WithColor(Color.Gold);

                    await command.RespondAsync(embed:  restoreResponse.Build());
                    break;

                case "clear":
                    _handler.ClearChatHistory();
                    sentInitialMessage = false;

                    var clearResponse = new EmbedBuilder()
                        .WithDescription("Deleted chat history.")
                        .WithColor(Color.Gold);

                    await command.RespondAsync(embed:  clearResponse.Build());
                    break;
            }
        }

        public async Task MessageReceivedAsync(SocketMessage message)
        {
            Random random = new Random();
            int num = random.Next(interjectionChance);
            string msgText = message.Content;

            tokens = _handler.CalculateTokens();
            while (tokens > 2500)
            {
                _handler.PurgeMemory();
                tokens = _handler.CalculateTokens();
            }

            if (message.Author.IsBot) { return; }

            string user;
            if (message.Author.GlobalName != null)
            {
                user = message.Author.GlobalName;
            }
            else
            {
                user = message.Author.Username;
            }

            string userMessage = $"[author: {user}]: {message.Content}";
            Console.WriteLine(userMessage + " - " + tokens);

            if (!sentInitialMessage)
            {
                sentInitialMessage = true;
                Console.WriteLine("Obtaining a response from ChatGPT...");

                using (message.Channel.EnterTypingState())
                {
                    string response = _handler.SendInitialMessage();

                    tokens = _handler.CalculateTokens();
                    Console.WriteLine($"[kevin]: {response} - {tokens}");

                    var myEmbed = QuickEmbeder(response);

                    await message.Channel.SendMessageAsync(embed: myEmbed.Build());
                }
            }

            _handler.AddChatHistory(userMessage);
            Console.WriteLine(num);

            if (message.Content.StartsWith("/chat"))
            {
                Console.WriteLine("Obtaining a response from ChatGPT...");

                using (message.Channel.EnterTypingState())
                {
                    string response = _handler.Respond();

                    tokens = _handler.CalculateTokens();
                    Console.WriteLine($"[kevin]: {response} - {tokens}");
                    lastAuthor = message.Author;

                    var myEmbed = QuickEmbeder(response);
                    await message.Channel.SendMessageAsync(message.Author.Mention + "\n", embed: myEmbed.Build()); ;
                }
            }
            else if (num == 0)
            {
                Console.WriteLine("Obtaining a response from ChatGPT...");

                using (message.Channel.EnterTypingState())
                {
                    string response = _handler.Respond();

                    tokens = _handler.CalculateTokens();
                    Console.WriteLine($"[kevin]: {response} - {tokens}");

                    lastAuthor = message.Author;

                    var myEmbed = QuickEmbeder(response);
                    await message.Channel.SendMessageAsync(message.Author.Mention + "\n", embed: myEmbed.Build());
                }
            }
        }

        EmbedBuilder QuickEmbeder(string input)
        {
            var returningEmebed = new EmbedBuilder()
                .WithDescription(input)
                .WithColor(Color.Red);

            return returningEmebed;
        }


        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task ReadyAsync()
        {
            await _client.SetGameAsync("with your fragile egos. Use /chat to draw my disdain.");
            
            Console.WriteLine($"{_client.CurrentUser.Username} is connected!");
        }
    }
}
