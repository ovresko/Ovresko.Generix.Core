using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Datasource.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Modules.Base
{
    public enum CreateModes
    {
        Create,
        CreateOpen,
        Submit,
        SubmitOpen,
        Save,
        SaveOpen
    }

    public class CreateModesHandler
    {
        public static IModel Handle(IModel es,CreateModes mode)
        {
            switch (mode)
            {
                case CreateModes.Create:
                    break;
                case CreateModes.CreateOpen:
                    es.Open(OpenMode.Detach);
                    break;
                case CreateModes.Submit:
                    es.Save();
                    es.Submit();
                    break;
                case CreateModes.SubmitOpen:
                    es.Save();
                    es.Submit();
                    es.Open(OpenMode.Detach);
                    break;
                case CreateModes.Save:
                    es.Save();
                    break;
                case CreateModes.SaveOpen:
                    es.Save();
                    es.Open(OpenMode.Detach);
                    break;
                default:
                    break;
            }

            return es;
        }
    }
}
