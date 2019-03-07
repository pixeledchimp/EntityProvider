using EntityProvider.Tests;

namespace EntityProvider.Tests
{
    public class ImplementedModel : IInterfaceModel
    {
        public string SomeProp1 { get; set; }

        public int SomeProp2 { get; set; }

        public ImplementedModel(string someprop1, int someprop2)
        {
            SomeProp1 = someprop1;
            SomeProp2 = someprop2;
        }

        public ImplementedModel()
        {
        }
    }
}