-- Create Author Table
CREATE TABLE Author (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL,
    Name NVARCHAR(100) NOT NULL
);

-- Create Thread Table
CREATE TABLE Thread (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200),
    Name NVARCHAR(200),
    Type NVARCHAR(50) NOT NULL
);

-- Create Message Table
CREATE TABLE Message (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200),
    Text NVARCHAR(1000) NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    AuthorId INT NOT NULL,
    ThreadId INT NOT NULL,
    FOREIGN KEY (AuthorId) REFERENCES Author(Id),
    FOREIGN KEY (ThreadId) REFERENCES Thread(Id)
);
