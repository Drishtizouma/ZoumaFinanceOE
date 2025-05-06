namespace ZoumaFinance
{
	public static class SD
	{
		public static string ZohoAPIBase { get; set; }
		public static string TokenAPIBase { get; set; }
        public static string DeskAPIBase { get; set; }


        //public static string ShoppingCartAPIBase { get; set; }
        //public static string CouponAPIBase { get; set; }
        public enum ApiType
		{
			GET,
			POST,
			PUT,
			DELETE
		}
	}
}
