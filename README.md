# Simple Transfer Application

## Description
A simple demo for deposite, withdraw, transfer amount for handling concurrency issues by ASP.NET Mvc Core.

## Installation
Edit file appsettings.json in src/SimpleBank.Web folder to edit the database connection sever.
Tables will be auto-generated for the first time running with simple default data.

## Techniques
 * Use simple Cookies Authentication of AspNetCore to authenticate existing User table.
 * Use EntityFrameworkCore.Timestamp to handle concurrency issues.
 * Use Xunit for the unit testing.
 * Use EntityFrameworkCore.InMemory as temp database for the INSERT/UPDATE operations for the unit testing.