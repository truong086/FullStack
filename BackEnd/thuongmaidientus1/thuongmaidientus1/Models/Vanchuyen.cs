namespace thuongmaidientus1.Models
{
    public class Vanchuyen : BaseEntity
    {
        public string? name { get; set; }
        public string? diachi { get; set; }
        public string? image { get; set; }

        public virtual IList<Shop>? Shops { get; set; }
        public virtual IList<ShopVanchuyen>? ShopVanchuyens { get; set; }
        public virtual IList<Xulydonhang>? Vanchuyens { get; set; }
    }
}
