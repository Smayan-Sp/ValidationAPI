namespace Validation_Management_API.Services.Results
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public Outcome ResultType { get; set; }

        public static ServiceResult<T> Create(bool success, Outcome outcome, T? data = default, string? message = null)
        {
            return new ServiceResult<T>
            {
                Success = success,
                Data = data,
                ResultType = outcome,
                Message = message
            };
        }

        public static ServiceResult<T> Ok(T data)
        {
            return Create(true, Outcome.Ok, data);
        }

        public static ServiceResult<T> CreatedAtAction(string message, T data)
        {
            return Create(true, Outcome.CreatedAtAction, data, message);
        }
        public static ServiceResult<T> NoContent()
        {
            return Create(true, Outcome.NoContent);
        }

        public static ServiceResult<T> BadRequest(string message)
        {
            return Create(false, Outcome.BadRequest, message: message);
        }

        public static ServiceResult<T> NotFound(string message)
        {
            return Create(false, Outcome.NotFound, message: message);
        }

        public static ServiceResult<T> Conflict(string message)
        {
            return Create(false, Outcome.Conflict, message: message);
        }

        public static ServiceResult<T> InternalServerError (string message)
        {
            return Create(false, Outcome.InternalServerError, message: message);
        }

    }

}
