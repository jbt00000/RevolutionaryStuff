CREATE proc [db].[AssertNull]
	@actual int,
	@msg varchar(4000)=null
as
begin

	if (@actual is not NULL)
	begin

		set @msg = 'AssertNull FAIL as value ['+cast(@actual as varchar(20))+'] is not null: '+@msg;
		throw 50001, @msg, 1;

	end

end

GO

CREATE proc [db].[AssertNotNull]
	@actual int,
	@msg varchar(4000)=null
as
begin

	if (@actual is NULL)
	begin

		set @msg = 'AssertNotNull FAIL as value is null: '+@msg;
		throw 50001, @msg, 1;

	end

end

GO

CREATE proc [db].[AssertZero]
	@actual int,
	@msg varchar(4000)=null
as
begin

	if (@actual not in (0))
	begin

		set @msg = 'AssertZero FAIL as ['+cast(@actual as varchar(10))+'] <> 0: '+@msg;
		throw 50001, @msg, 1;

	end

end

GO

CREATE proc [db].[AssertZeroOrOne]
	@actual int,
	@msg varchar(4000)=null
as
begin

	if (@actual not in (0,1))
	begin

		set @msg = 'AssertZeroOrOne FAIL as ['+cast(@actual as varchar(10))+'] not in (0,1): '+@msg;
		throw 50001, @msg, 1;

	end

end

GO

CREATE proc [db].[AssertEquals]
	@expected sql_variant,
	@actual sql_variant,
	@msg varchar(4000)=null
as
begin

	if (
		(@expected is not null and (@actual is null or @expected<>@actual)) or
		(@expected is null and @actual is not null)
		)
	begin

		set @msg = 
			'AssertEquals FAIL as '+
			case when @actual is null then 'null' else '['+cast(@actual as varchar(max))+']' end +
			' <> '+
			case when @expected is null then 'null' else '['+cast(@expected as varchar(max))+']' end +
			': '+
			coalesce(@msg,'');			
		throw 50001, @msg, 1;

	end

end

GO

CREATE proc [db].[AssertFail]
	@msg varchar(4000)=null
as
begin

	set @msg = 'AssertFail: '+@msg;
	throw 50001, @msg, 1;

end

GO

CREATE proc [db].[AssertStringHasContent]
	@test nvarchar(max),
	@msg varchar(4000)=null
as
begin

	if (@test is not null) set @test=ltrim(rtrim(@test))
	if (@test is null or len(@test)=0)
	begin
		set @msg = 'AssertStringHasContent: Failed: '+@msg;
		throw 50001, @msg, 1;
	end

end

GO
