using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CRMS.Business.Services.DocumentService
{
    public interface IDocumentConverter
    {
        string FlowDocumentToHtml(FlowDocument doc);
    }
}
