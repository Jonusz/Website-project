# Group Project: Car Rental Website

This is a group project created as part of the **Instituto Superior de Engenharia de Coímbra** studies. The project is a web application designed using the **Model-View-Controller (MVC)** architecture.

## Project Overview

The goal of the project was to build a **car rental website** where users can log in with accounts that have different access permissions. The system includes the following user roles:

- **Administrator**
- **Manager**
- **Employee**
- **Customer**

Each role has distinct access to specific functionalities based on the permissions granted.

## Features by User Role

- **Administrator**: Full access to manage all aspects of the system.
- **Manager**: Manages employees and car inventory.
- **Employee**: Can assist with managing cars and processing reservations.
- **Customer**: Can view available cars and make reservations.

## How to Run the Project

To run the project locally, follow these steps:

1. **Connect the Database**:  
   Use the attached database file and link it to your local server.

2. **Create Database Migration**:
   - Open the **Package Manager Console** (Tools > NuGet Package Manager > Package Manager Console).
   - Run the following commands:
     ```bash
     Add-Migration InitialCreate
     Update-Database
     ```

3. **Administrator Login Credentials**:  
   - **Email**: admin@localhost.com  
   - **Password**: Erasmus_123

![animacja](https://github.com/user-attachments/assets/e507b455-cffe-4389-9d05-d2183d1ba4b4)
