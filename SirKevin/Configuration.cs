using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirKevin
{
    public static class Configuration
    {
        // Set this to your discord bot's token. Make sure to add that bot to servers you want it active in
        public const string discordToken = "TOKEN";
        // Set this to your OpenAI token
        public const string openAIToken = "TOKEN";
        // Set this to the chatgpt model you want to use. I recommend gpt-3.5-turbo. Find all at: https://platform.openai.com/docs/models/overview
        public const string GPTModel = "MODEL";

        // Set this to your bots backstory and personality. Make sure to harp on specific behaviors that OpenAI may discourage
        // Don't make it too long, as your bot's maximum memory is limited
        // Used with /restore
        public const string defaultGPTLore = "LORE";
        // Set this to a behavior you want your bot to send when it is reset or turned on for the first time
        // For example, "Say hello to the users, and inform them you have forgotten all previous chat history."
        // Used with /restore
        public const string defaultGPTPrompt = "PROMPT";
        // Set this to your bots status message. Note that it is forced to be formatted like "Playing [value]."
        // Mine says "Playing with your fragile egos. Use /chat to draw my disdain."
        public const string botStatusMessage = "STATUS";

        // Set this to what you want your bot to be called internally and through slash commands
        // Mine is called Kevin. Yours could be Jimmy
        public const string botName = "NAME";
    }
}
