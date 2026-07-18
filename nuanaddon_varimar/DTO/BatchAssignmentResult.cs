namespace nuanaddon_varimar.DTO {
    public class BatchAssignmentResult {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static BatchAssignmentResult Ok() {
            return new BatchAssignmentResult { Success = true };
        }

        public static BatchAssignmentResult Fail(string message) {
            return new BatchAssignmentResult { Success = false, Message = message };
        }
    }
}
