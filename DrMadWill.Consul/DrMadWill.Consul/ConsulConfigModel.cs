namespace DrMadWill.Consul
{
    public class ConsulConfigModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ConsulAddress { get; set; }
        public string Address { get; set; }
        public string[] Tags => new[] { Name, Id };

        public override string ToString()
        {
            return $" ::>> Config =>>> ID : {Id} | Name : {Name} | ConsulAddress : {ConsulAddress} | Address : {Address} ";
        }
    }
}