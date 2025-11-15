# Core Banking Solution

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/) 
[![C#](https://img.shields.io/badge/C%23-8.0-green)](https://learn.microsoft.com/en-us/dotnet/csharp/) 
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-yellow)](LICENSE)

A **robust backend solution for core banking operations**, built using **ASP.NET Core 8**, **C#**, and **PostgreSQL**, following the **Clean Architecture** principles. This project demonstrates a modular, maintainable, and secure backend system suitable for financial applications.

---

## Table of Contents

- [Overview](#overview)  
- [Features](#features)  
- [Architecture](#architecture)  
- [Folder Structure](#folder-structure)  
- [Tech Stack](#tech-stack)  
- [Installation & Setup](#installation--setup)  
- [Database Setup](#database-setup)  
- [API Documentation](#api-documentation)  
- [Project Showcase](#project-showcase)  
- [Validation & Security](#validation--security)  
- [Testing](#testing)  
- [Contributing](#contributing)  
- [License](#license)  
- [Author](#author)

---

## Overview

The **Core Banking Solution** backend allows management of banking operations, including customer accounts, transactions, and user roles. It demonstrates:

- Clean architecture separation: Domain, Application, Infrastructure, Presentation  
- Secure authentication & authorization with **ASP.NET Core Identity** and **JWT**  
- RESTful API with HATEOAS support for scalable services  
- Automated validation pipelines and error handling  

---

## Features

- **Customer Management:** Register, update, and retrieve customer information  
- **Bank Accounts:** Create, close, and manage multiple accounts per customer  
- **Transactions:** Deposit, withdrawal, transfer, and transaction history  
- **Role-based Access Control:** Admin, Teller, and Customer roles  
- **Validation Pipeline:** Ensures DRY principles, input validation, and domain rules  
- **Security:** Password hashing, JWT authentication, and claims-based authorization  
- **Logging & Auditing:** Tracks critical actions for accountability  

---

## Architecture

```mermaid
flowchart TB
    A[Presentation Layer: API Controllers] --> B[Application Layer: Services & Use Cases]
    B --> C[Domain Layer: Entities & Interfaces]
    C --> D[Infrastructure Layer: EF Core, Identity, Repositories]
    D --> E[PostgreSQL Database]
