using Autofac;
using CITChat.Translators;

namespace CITChat.Controllers
{
    /// <summary>
    /// </summary>
    public static class ContentTranslatorHelper
    {
        /// <summary>
        /// </summary>
        public static IContentTranslator ContentTranslator
        {
            get
            {
                var container = ContainerManager.Container;
                IContentTranslator contentTranslator = container.Resolve<IContentTranslator>();
                return contentTranslator;
            }
        }
    }
}