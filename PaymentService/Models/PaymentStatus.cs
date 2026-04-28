namespace PaymentService.Models
{
    public enum PaymentStatus
    {
        Pending,          
        Processing,       
        Completed,        
        Failed,           
        Declined,         
        Cancelled,        
        Refunded          
    }
}