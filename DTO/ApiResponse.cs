namespace AdWebService.DTO
{
    public class ApiResponse
    {
        public bool IsSuccess { get; set; } = false;
        public string ErrorMessage { get; set; }
        public object Response { get; set; }
    }
}
