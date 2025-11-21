Part 3 focused on building out the ASP.NET Core MVC structure of the project. The goal was to establish a clean separation of concerns between Models, Views, and Controllers, ensuring the application is both maintainable and scalable.

Key highlights:

Controllers: Implemented role-specific controllers (e.g., HRController, LecturerController, AdminController) to handle business logic and enforce session-based access control.

Models: Defined strong data models for entities like Lecturer and Claim, ensuring consistent mapping between the database and application logic.

Views: Created Razor views for dashboards, claim submissions, and management pages, with dynamic data passed through ViewBag and ViewData.

Navigation & Access Control: Integrated session checks and role-based redirects to secure different parts of the system.

Error Handling & Feedback: Used TempData and validation messages to provide clear feedback to users during workflows like claim submission and lecturer creation.

This part established the core MVC foundation of the project, enabling structured workflows and preparing the system for further enhancements like reporting and admin tools. 

Youtube link: https://youtu.be/FYwt7YjMejA

Presentation: "C:\Users\lab_services_student\Desktop\PROG6212 POE Part 3 Presentation\PROG6212 POE Part 3 Presentation.pptx"

