using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DemoPhotoBooth.Communicate
{
    public class MessageTypes
    {
        public class Page1ToPage2 : PubSubEvent<dynamic>
        {

        }

        public class Page2ToPage1 : PubSubEvent<dynamic>
        {

        }

        public class ResetMessage : PubSubEvent<dynamic>
        {

        }

        public class PreviewPageNextPage : PubSubEvent<string>
        {

        }

        public class SendVisual : PubSubEvent<Visual>
        {

        }
    }
}
