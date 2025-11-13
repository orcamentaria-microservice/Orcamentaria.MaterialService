namespace Orcamentaria.MaterialService.Domain.DTOs.Person
{
    public class PersonResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Rg { get; set; }
        public string Cpf { get; set; }
        public string Cnpj { get; set; }
        public bool Active { get; set; }
        public bool IsFromCache { get; set; } = false;
    }
}
