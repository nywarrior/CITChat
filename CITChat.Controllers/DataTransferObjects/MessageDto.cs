using CITChat.Models;
using CITChat.Translators;

namespace CITChat.Controllers.DataTransferObjects
{
    /// <summary>
    ///     Data transfer object for <see cref="Message" />
    /// </summary>
    public sealed class MessageDto
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public string TranslatedContent { get; set; }
        public bool IsDone { get; set; }
        public int ConversationId { get; set; }

        public MessageDto()
        {
        }

        public MessageDto(Message item)
        {
            MessageId = item.MessageId;
            Content = item.Content;
            IContentTranslator contentTranslator = ContentTranslatorHelper.ContentTranslator;
            TranslatedContent = contentTranslator.TranslateContent(item.Content);
            IsDone = item.IsDone;
            ConversationId = item.ConversationId;
        }

        public Message ToEntity()
        {
            IContentTranslator contentTranslator = ContentTranslatorHelper.ContentTranslator;
            TranslatedContent = contentTranslator.TranslateContent(Content);
            return new Message
                {
                    ConversationId = ConversationId,
                    MessageId = MessageId,
                    Content = Content,
                    TranslatedContent = TranslatedContent,
                    IsDone = IsDone,
                };
        }

        public void UpdateEntity(Message message)
        {
            IContentTranslator contentTranslator = ContentTranslatorHelper.ContentTranslator;
            TranslatedContent = contentTranslator.TranslateContent(Content);
            message.Content = Content;
            message.TranslatedContent = TranslatedContent;
            message.IsDone = IsDone;
        }
    }
}