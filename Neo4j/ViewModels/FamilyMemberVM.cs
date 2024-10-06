using Neo4j.Models;

namespace Neo4j.ViewModels
{
    public class FamilyMemberVM
    {
        public UserModel User { get; set; }
        public string Relation { get; set; }
    }
}
