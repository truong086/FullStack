namespace thuongmaidientus1.Models
{
    public class ShopVanchuyen : BaseEntity
    {
        public Shop? shop { get; set; }  
        public Vanchuyen? Vanchuyen { get; set; }
    }
}
