using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace e_shop.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public int? BrandFid { get; set; }

    public int? CategoryFid { get; set; }

    public decimal? Pprice { get; set; }

    public decimal? Srice { get; set; }

    public int? StockQuantity { get; set; }

    public int? Rating { get; set; }

    public string? Image { get; set; }

    public string? Details { get; set; }

    public string? Status { get; set; }
    
    [JsonIgnore]
    
    public virtual Category? CategoryF { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
