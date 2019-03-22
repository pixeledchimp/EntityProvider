using System;
using System.Collections.Generic;
using System.Text;

namespace EntityProvider.Tests
{
    class OtherImplementedModel: IInterfaceModel
    {
        public string SomeProp1 { get; set; }

        public int SomeProp2 { get; set; }

        public OtherImplementedModel()
        {
            SomeProp1 = this.GetType().ToString();
        }
    }
}
