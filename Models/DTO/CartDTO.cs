﻿namespace CT554_API.Models.DTO
{
    public class CartDTO
    {
        public string? Id { get; set; }
        public float Quantity { set; get; } = 1;
        public float RealQuantity { set; get; } = 0;
        public int ProductDetailId { set; get; }
        public string? UserId { set; get; }

    }
}
