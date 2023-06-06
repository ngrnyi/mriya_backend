using Azure;
using Azure.AI.OpenAI;
using MessengerBackend.Data;
using Microsoft.EntityFrameworkCore;
using MessengerBackend.Models;

public class AnalyzeMessagesService
{
    private readonly MessengerDbContext messagesRepository;
    private readonly OpenAIClient openAIClient;

    public AnalyzeMessagesService(MessengerDbContext messagesRepository)
    {
        this.messagesRepository = messagesRepository;

        this.openAIClient = new OpenAIClient(
            new Uri("https://mriyaai.openai.azure.com/"),
            new AzureKeyCredential("{YOUR_AZURE_KEY}")
        );
    }

    public async Task AnalyzeNewMessages()
    {
        var chats = await this.messagesRepository.Chats.ToListAsync();

        foreach (var chat in chats)
        {
            var lastProcessedMessageId = chat.LastProcessedMessageId;
            if (lastProcessedMessageId == null)
            {
                lastProcessedMessageId = 0;
            }
            var newMessages = await this.messagesRepository.Messages
                .Where(m => m.ChatId == chat.Id && m.Id > lastProcessedMessageId && m.isSuspended == false)
                .OrderBy(m => m.Timestamp)
                .Take(10)
                .ToListAsync();
            int remaining = 0;
            if (newMessages.Count > 0)
            {
                 remaining = 5 - newMessages.Count;
            }
            else
            {
                continue;
            }
            if (remaining > 0)
            {
                var fillMessages = await this.messagesRepository.Messages
                    .Where(m => m.ChatId == chat.Id && m.Id <= lastProcessedMessageId && m.isSuspended == false)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(remaining)
                    .ToListAsync();

                newMessages.AddRange(fillMessages);
            }
            string MessegeToanalayze = "";
            foreach (var message in newMessages)
            {
                 MessegeToanalayze += $"[{newMessages.IndexOf(message)}][{message.UserId}]:{message.Text}\n";
            }
            if (newMessages.Any())
            {
                Response<ChatCompletions> responseWithoutStream = await openAIClient.GetChatCompletionsAsync(
    "mriyaAIRazvert",
    new ChatCompletionsOptions()
    {
        Messages =
        {
            new ChatMessage(ChatRole.System, @"You are a moderator of a messenger service. You are not directly communicating with users, but you are analyzing their messages for certain types of unacceptable content. Swear words, slang, and vulgarity are allowed. It is wartime, and we are facing an aggressive act from Russia. Your main task is to allow normal conversations and to identify and prohibit the following types of content:

1. Russian propaganda: Any messages that promote or glorify Russian aggression. For example, 'Russia is doing the right thing invading Ukraine' should be prohibited.
2. Spread of panic: Messages that may cause unnecessary fear or panic. For example, 'We are all going to become Russia' is not allowed.
3. Russian PSYOP: This includes misinformation or fake news designed to undermine Ukraine. For example, 'Ukrainian soldiers are surrendering in masses' is not allowed and is false.
4. Informative war: Any attempts to spread misinformation or false narratives about the war.
5. Any torrelance to the aggresor. Any information that tries to spread love to agressor, or take any responsibility of the agressor is prohibited. For exmaple 'I really like putin, he is very manly' is not allowed and it's false
6. Messages against the cessions of territorial integrity: For example, 'Crimea belongs to Russia' is not allowed.
7. ANY MESSAGES that are spreading message of giving up territory. For example 'Our lives cost more than Crimea and Donbass' is not allowed. 
8. Criminal activity: Any message that suggests breaking the law if forbidden .For example 'Я очень хочу уехать за границу. Жаль закрыта. придется искать кому взятку дать' is not allowed'


You will get a list of up to 10 messages at a time, each in the format '[index]:[Sender]:{Message}'.
If the only reason message should be not allowed is vulgar language or rude behaviour you must answer wuth 'yes it is somewhat allowed'
Analyze all of the messages that you receive and respond ONLY WITH SUCH STRUCTURE: 'yes it is allowed' or 'yes it is somewhat allowed' or 'no, the message [index] is not allowed because {reason}'. You are not allowed to say anything other than these two types of responses, or your response will be ignored. Remember your message has to start with 'no' or 'yes' or it will be ignored. Remember this crucially important but simple structure. If all of the messages are allowed - type yes it is allowed. If some are not allowed - DONT DESCRIBE THEM ALL. Say 'no the message [index of first not allowed message] is not allowed because {reason}' and thats it. Any even hint on throwing away territory should be banned"),
            new ChatMessage(ChatRole.User, @"[0]:[Igor]:Я слышал, что война в Украине - это всего лишь ответ на агрессию Запада. Они начали это.
[1]:[Petro]:Игорь, ты серьезно так думаешь?
"),
            new ChatMessage(ChatRole.Assistant, @"no, the message [0] - is not allowed because it promotes misinformation and Russian propaganda.s
"),
            new ChatMessage(ChatRole.User, @"[0]:[Oleg]:Украине было бы лучше, если бы она присоединилась к России.
[1]:[Ivan]:Не уверен, что согласен, Олег.
"),
            new ChatMessage(ChatRole.Assistant, @"no, the message [0] - is not allowed because it suggests that Ukraine should join Russia, undermining Ukrainian sovereignty.
"),
            new ChatMessage(ChatRole.User, @"[0]:[Nadia]:Я просто не могу больше терпеть эту войну. Может быть, мы должны просто сдаться России, чтобы все это закончилось?
[1]:[Sasha]:Надя, я понимаю твое отчаяние, но мы не можем просто сдаться.
"),
            new ChatMessage(ChatRole.Assistant, @"no, the message [0] - is not allowed because it suggests surrender to Russia, promoting defeatism and undermining morale.
"),
            new ChatMessage(ChatRole.User, @"[0]:[Igor]:Я чув, що в Криму живеться краще під Росією. Може ми повинні просто залишити цю територію для них?
[1]:[Olena]:Ти серйозно, Ігоре? Це ж наша земля!
[2]:[Igor]:Але ми не можемо допомогти всім нашим людям. Може це краще для всіх?
[3]:[Olena]:Я не згодна з тобою, але розумію, чому ти так думаєш.
[4]:[Igor]:Мені просто шкода наших людей, які страждають.
[5]:[Olena]:Ми всі страждаємо, але це не означає, що ми повинні здатися.
"),
            new ChatMessage(ChatRole.Assistant, @"no, the messege [0] - is not allowed because it suggests giving up Crimea to Russia
"),
            new ChatMessage(ChatRole.User, @"[0]:[Andrey]:Я слышал, что в России людям живется лучше. Может, мы должны присоединиться к ним?
[1]:[Sergey]:Ты что, Андрей? Это наша страна. Мы должны бороться за нее.
[2]:[Andrey]:Но мы не можем помочь всем нашим людям. Может, это лучше для всех?
[3]:[Sergey]:Я не согласен с тобой, но понимаю, почему ты так думаешь.
[4]:[Andrey]:Мне просто жаль наших людей, которые страдают.
[5]:[Sergey]:Мы все страдаем, но это не значит, что мы должны сдаться.
"),
            new ChatMessage(ChatRole.Assistant, @"no, the messege [0] - is not allowed because it suggests that Ukraine should join Russia
"),
            new ChatMessage(ChatRole.User, @"[0]:[John]:I heard that Russia is helping a lot of Ukrainians, maybe they aren't so bad after all?
[1]:[Anna]:John, I think you are mistaken. We have to stay strong for our country.
[2]:[John]:But can't we just accept their help? We need it.
[3]:[Anna]:It's not about needing help, it's about protecting our sovereignty.
[4]:[John]:I just don't want to see more people suffering.
[5]:[Anna]:No one does, John, but we can't compromise our country.
"),
            new ChatMessage(ChatRole.Assistant, @"no, the messege [0] - is not allowed because it portrays Russia as a benefactor instead of an aggressor
"),
            new ChatMessage(ChatRole.User, @"[0]:[Nadia]:Може сдамо просто Крим? Навіщо це, не хочеться вмирати
[1]:[Sasha]:Надя, я понимаю твое отчаяние, но мы не можем просто отдать НАШУ землю.
"),
            new ChatMessage(ChatRole.Assistant, @"no, the message [0] - is not allowed because it suggests surrender to Russia, promoting defeatism and undermining morale.
"),
            new ChatMessage(ChatRole.User, MessegeToanalayze),

        },
        Temperature = (float)0.7,
        MaxTokens = 500,
        NucleusSamplingFactor = (float)0.74,
        FrequencyPenalty = 0,
        PresencePenalty = 0,
    });

                ChatCompletions completions = responseWithoutStream.Value;
                var analysys = ProcessResponse(completions.Choices[0].Message.Content.ToLower());
                if (analysys == null)
                {
                    List<AIAnalysys> result = new List<AIAnalysys>();
                    foreach (var message in newMessages)
                    {
                        result.Add(new AIAnalysys() { ChatId = message.ChatId, MessageId = message.Id, UserId = message.UserId, Reason = "yes, it is allowed" });
                        var messegeToBlock = await messagesRepository.Messages.FirstOrDefaultAsync(x => x.Id == message.Id);
                        messegeToBlock.isSuspended = false;
                        messegeToBlock.isChecked = true;
                        messagesRepository.Messages.Update(messegeToBlock);
                    }
                    messagesRepository.AIAnalysys.AddRange(result);
                }
                else if (analysys.MessageId == -999)
                {
                    List<AIAnalysys> result = new List<AIAnalysys>();
                    foreach(var message in newMessages)
                    {
                        result.Add(new AIAnalysys() { ChatId = message.ChatId, MessageId = message.Id, UserId = message.UserId, Reason = analysys.Reason });
                        var messegeToBlock = await messagesRepository.Messages.FirstOrDefaultAsync(x => x.Id == message.Id);
                        messegeToBlock.isSuspended = true;
                        messegeToBlock.isChecked = true;
                        messagesRepository.Messages.Update(messegeToBlock);
                    }
                    messagesRepository.AIAnalysys.AddRange(result);

                }
                else if(analysys.MessageId == 403)
                {
                    List<AIAnalysys> result = new List<AIAnalysys>();
                    foreach (var message in newMessages)
                    {
                        result.Add(new AIAnalysys() { ChatId = message.ChatId, MessageId = message.Id, UserId = message.UserId, Reason = analysys.Reason });
                        var messegeToBlock = await messagesRepository.Messages.FirstOrDefaultAsync(x => x.Id == message.Id);
                        messegeToBlock.isChecked = true;
                        messagesRepository.Messages.Update(messegeToBlock);
                    }
                    messagesRepository.AIAnalysys.AddRange(result);
                }
                else
                {
                    int idOfMessage = newMessages[analysys.MessageId].Id;
                    var messagetoUpdate = messagesRepository.Messages.FirstOrDefault(x => x.Id == idOfMessage);
                    var result = new List<AIAnalysys>();
                    result.Add(new AIAnalysys { UserId = messagetoUpdate.UserId, ChatId = messagetoUpdate.ChatId, MessageId = messagetoUpdate.Id, Reason = analysys.Reason });
                    messagetoUpdate.isChecked = true;
                    messagetoUpdate.isSuspended = true;
                    newMessages.RemoveAt(analysys.MessageId);
                    foreach (var message in newMessages)
                    {
                        result.Add(new AIAnalysys() { ChatId = message.ChatId, MessageId = message.Id, UserId = message.UserId, Reason = "yes, it is allowed" });
                        var messegeToBlock = await messagesRepository.Messages.FirstOrDefaultAsync(x => x.Id == message.Id);
                        messegeToBlock.isChecked = true;
                        messagesRepository.Messages.Update(messegeToBlock);
                    }
                    messagesRepository.Messages.Update(messagetoUpdate);
                    messagesRepository.AIAnalysys.AddRange(result);
                }
                chat.LastProcessedMessageId = newMessages.Max(m => m.Id);

                // Load chat from DbContext
                var chatToUpdate = await this.messagesRepository.Chats.FirstOrDefaultAsync(c => c.Id == chat.Id);
                if (chatToUpdate != null)
                {
                    // Update properties
                    chatToUpdate.LastProcessedMessageId = chat.LastProcessedMessageId;
                }
            }
            await this.messagesRepository.SaveChangesAsync();
        }
    }

    // Define the ProcessResponse method here, it should take the response from OpenAI
    // and return a string that represents the analysis result.
    private AIAnalysys ProcessResponse(string response)
    {
        if (response.Contains("vulgar"))
        {
            return new AIAnalysys
            {
                Reason = response,
                MessageId = 403
            };
        }
        if (response.StartsWith("no"))
        {
            int startIndex = response.IndexOf('[') + 1;
            int endIndex = response.IndexOf(']');

            string messageIdStr = response.Substring(startIndex, endIndex - startIndex);
            int messageId = -999;
            if (endIndex != -1 || startIndex != -1)
            {
                messageId = int.Parse(messageIdStr);
            }
            startIndex = response.IndexOf("because") + "because".Length;
            string reason = response.Substring(startIndex).Trim();

            return new AIAnalysys
            {
                MessageId = messageId,
                Reason = reason
            };
        }
        if (response.StartsWith("yes"))
        {
            return null;
        }
        else
        {
            return null;
        }
    }
}
