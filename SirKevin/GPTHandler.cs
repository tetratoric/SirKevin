using Azure.AI.OpenAI;
using SirKevin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace KevinTheCrisp
{
    internal class GPTHandler
    {
        OpenAIClient client = new OpenAIClient(Configuration.openAIToken);
        
        string model = Configuration.GPTModel;
        List<ChatMessage> chatHistory = new List<ChatMessage>();

        internal string currentLore = "";
        internal string currentDefaultPrompt = "";

        bool initialStart = true;

        internal void AddChatHistory(string msg)
        {
            ChatMessage chatMessage = new ChatMessage(ChatRole.User, msg);
            chatHistory.Add(chatMessage);
        }
        
        internal string SendInitialMessage()
        {
            if (initialStart)
            {
                currentLore = Configuration.defaultGPTLore;
                currentDefaultPrompt = Configuration.defaultGPTPrompt;
                initialStart = false;
            }
            
            chatHistory.Clear();
            
            AddChatHistory(currentLore);
            AddChatHistory(currentDefaultPrompt);
            
            ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions(chatHistory);

            ChatMessage response = client.GetChatCompletions(model, chatCompletionsOptions).Value.Choices[0].Message;
            chatHistory.Add(response);

            string responseMsg = response.Content;
            return responseMsg;
        }

        internal string Respond()
        {
            ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions(chatHistory);

            ChatMessage response = client.GetChatCompletions(model, chatCompletionsOptions).Value.Choices[0].Message;
            chatHistory.Add(response);

            string responseMsg = response.Content;
            return responseMsg;
        }

        internal void PurgeMemory()
        {
            chatHistory.RemoveAt(1);
        }

        internal int CalculateTokens()
        {
            var encoding = Tiktoken.Encoding.ForModel(model);

            int numberOfTokens = 0;

            foreach (var chat in chatHistory) {
                numberOfTokens += encoding.CountTokens(chat.Content);
            }

            return numberOfTokens;
        }

        internal void ClearChatHistory()
        {
            chatHistory.Clear();
        }

        internal void RestoreBot()
        {
            currentLore = Configuration.defaultGPTLore;
            currentDefaultPrompt = Configuration.defaultGPTPrompt;
        }
    }
}
