namespace Core.Domain.Entities
{

    public class UnitAddress
    {
        public int Id { get; set; }

        public int  UnitId { get; set; }

        public int  CountryId { get; set; }

        public int StateId { get; set; }

        public int CityId { get; set; }

        public string? AddressLine1 { get; set; }


        public string? AddressLine2 { get; set; }

        public int PinCode  { get; set; }
        
    
        public string?  ContactNumber { get; set; }

      
        public string?  AlternateNumber { get; set; }

         public Unit? Unit { get; set; }
         

    }
}