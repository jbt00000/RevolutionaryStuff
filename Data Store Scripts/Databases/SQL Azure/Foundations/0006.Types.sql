create type EmailAddress from varchar(254) null
GO
CREATE TYPE DeveloperName FROM varchar(80) NOT NULL
GO
create type Percentage from float null
GO
create type Url from nvarchar(1024) null
GO
create type Title from nvarchar(255) null
GO
create type AspNetId from nvarchar(450) null
GO
create type RowStatus from char(1) not null
GO
create type JsonObject from nvarchar(max) null
GO
create type ForeignIdType from nvarchar(50) null
GO
CREATE TYPE [dbo].[IntListType] AS TABLE(
	[Val] [int] NULL,
	[Pos] [int] NULL
)

GO

CREATE TYPE [dbo].[StringListType] AS TABLE(
	[Val] [nvarchar](max) NULL,
	[Pos] [int] NULL
)

GO

CREATE TYPE [dbo].[BigIntListType] AS TABLE(
	[Val] [bigint] NULL,
	[Pos] [int] NULL
)

GO

CREATE TYPE [dbo].[GuidListType] AS TABLE(
	[Val] [uniqueidentifier] NULL,
	[Pos] [int] NULL
)

GO

CREATE TYPE [dbo].[DateListType] AS TABLE(
	[Val] [datetime] NULL,
	[Pos] [int] NULL
)

GO
