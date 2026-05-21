using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiShoppingAssistant.Models;

[Table("stores")]
public class Store
{
    [Column("id")]
    public int Id { get; set; }

    [Column("store_name")]
    public string StoreName { get; set; } = null!;
}

[Table("products")]
public class Product
{
    [Column("id")]
    public int Id { get; set; }

    [Column("store_id")]
    public int StoreId { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = null!;

    [Column("brand")]
    public string? Brand { get; set; }

    [Column("weight_g")]
    public int? WeightG { get; set; }

    [Column("price")]
    public decimal Price { get; set; }
}

[Table("search_history")]
public class SearchHistory
{
    [Column("id")]
    public int Id { get; set; }

    [Column("user_query")]
    public string UserQuery { get; set; } = null!;

    [Column("ai_response")]
    public string AiResponse { get; set; } = null!;

    [Column("saved_at")]
    public DateTime SavedAt { get; set; }
}
