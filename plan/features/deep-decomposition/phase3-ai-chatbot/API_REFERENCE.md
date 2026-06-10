# 📖 API Reference Details - Gemini AI Chatbot Advisor

## 1. POST /api/ai-chatbot/ask
Gửi tin nhắn hiện tại và lịch sử trò chuyện để nhận câu trả lời tư vấn tự động từ AI.

*   **Auth:** `[Authorize(Roles = "customer")]` (Chỉ chấp nhận khách hàng đã đăng nhập).
*   **Request Body:**
    ```json
    {
      "history": [
        {
          "role": "user",
          "content": "Con mèo của tôi bị rụng lông nhiều"
        },
        {
          "role": "model",
          "content": "Rụng lông ở mèo có thể do thay lông sinh lý, dinh dưỡng thiếu chất hoặc ký sinh trùng. Bạn có thấy bé ngứa ngáy hay gãi nhiều không?"
        }
      ],
      "message": "Có, bé gãi tai rất nhiều và da hơi đỏ."
    }
    ```
*   **Response (200 OK):**
    ```json
    {
      "response": "Nếu bé gãi nhiều ở vùng tai kèm đỏ da, rất có khả năng bé bị nhiễm rận tai hoặc nấm da. Bạn nên kiểm tra xem tai bé có các vết vảy đen hay không. Tuy nhiên, thông tin này chỉ mang tính tham khảo. Bạn nên mang bé tới MyPetClinic để bác sĩ thú y của chúng tôi khám trực tiếp nhé!"
    }
    ```
*   **Response (429 Too Many Requests):**
    ```json
    {
      "message": "Bạn đã vượt quá giới hạn câu hỏi tư vấn trong 1 phút. Vui lòng đợi và thử lại sau."
    }
    ```
