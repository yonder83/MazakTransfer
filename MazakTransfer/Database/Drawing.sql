CREATE TABLE [Drawing] (
    [Id] bigint IDENTITY (1,1) NOT NULL,
    [FileName] nvarchar(20)  NOT NULL,
    [Comment] nvarchar(4000)  NULL
);
GO

ALTER TABLE [Drawing] ADD CONSTRAINT [PK_Drawing] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Drawing] ADD CONSTRAINT [UQ_Drawing] UNIQUE ([FileName]);
GO
