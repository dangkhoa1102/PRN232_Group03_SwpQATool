# SWP QA Tool - Postman Test Guide

## Setup

### 1. Import Files
- Import `SWP_QATool_HappyCase.postman_collection.json`
- Import `SWP_QATool_Local.postman_environment.json`

### 2. Set Environment Variables
Mở `SWP_QATool_Local` environment và điền:
```
admin_email: admin@swp.com
admin_password: 123456

supervisor_email: supervisor1@swp.com
supervisor_password: 123456

reviewer_email: reviewer1@swp.com
reviewer_password: 123456

student_email: student1@swp.com
student_password: 123456
```

### 3. Select Environment
Trước khi chạy, chọn dropdown environment `SWP_QATool_Local`

### 4. Run Collection
- Click Collection → Run (Collection Runner)
- Hoặc click Play icon và Run

## Test Structure

**00 - Auth Setup** - Login 4 role, lưu token vào environment
**01 - Users** - CRUD user
**02 - Topics** - CRUD topic
**03 - Groups** - CRUD group + add/remove member
**04 - Admin History** - Xem lịch sử
**05 - Student** - Tạo Q&A, manage notification
**06 - Supervisor** - Duyệt/reject câu hỏi
**07 - Student Update Rejected** - Student cập nhật lại câu từ chối
**08 - Reviewer** - Trả lời câu hỏi

## Troubleshooting

### Lỗi Status 400 Login
- Kiểm tra environment variables có fill đủ không
- Verify email/password từ SQL Server:
```sql
SELECT email, role FROM users LIMIT 10;
```

### Lỗi "Has accessToken | expected undefined to exist"
- Chắc chắn **xóa collection cũ** rồi **import lại** file JSON mới
- Postman cache, có thể cần reload

### Lỗi 404 Not Found
- Check API container có chạy không: `docker compose ps`
- Restart: `docker compose down && docker compose up -d`

### Lỗi Unauthorized (401)
- Xác nhận token đã được save trong environment
- Check "Auth Me" request pass để verify token valid

## API Endpoints

- Base URL: `http://localhost:5192`
- Auth: `/api/auth/login`, `/api/auth/me`
- Users (Admin): `/api/users`
- Topics (Admin): `/api/topics`
- Groups (Admin): `/api/groups`
- Student: `/api/student/questions`, `/api/student/notifications`
- Supervisor: `/api/supervisor/questions`
- Reviewer: `/api/reviewer/questions`
- Admin: `/api/admin/history`

## Data Used

Tất cả test dùng dữ liệu **thực từ SQL Server**:
- Không mock, không fake response
- Create question, update, delete trong luồng test end-to-end
- Các ID được capture từ response của request trước đó

## Notes

- Luồng test thiết kế để chạy **tuần tự từ trên xuống**
- Một số request phụ thuộc ID từ request trước (vd: Update user phụ thuộc Create user)
- Environment variables được populate tự động qua test scripts
- Token hết hạn sau 480 phút (8 giờ)
