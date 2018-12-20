CREATE proc [db].[PrintNow]
	@format nvarchar(max),
	@n0 int=null,
	@n1 int=null,
	@n2 int=null,
	@n3 int=null,
	@n4 int=null,
	@n5 int=null,
	@n6 int=null,
	@n7 int=null,
	@s0 nvarchar(max)=null,
	@s1 nvarchar(max)=null,
	@s2 nvarchar(max)=null,
	@s3 nvarchar(max)=null,
	@d0 datetime=null,
	@d1 datetime=null,
	@d2 datetime=null,
	@d3 datetime=null,
	@b0 bit=null,
	@b1 bit=null,
	@b2 bit=null,
	@b3 bit=null,
	@m0 money=null,
	@m1 money=null,
	@m2 money=null,
	@m3 money=null,
	@startedAt datetime=null
as
begin

	declare @msg nvarchar(max) = @format
	
	set @msg = replace(@msg, '{n0}', coalesce(cast(@n0 as nvarchar(20)), ''))
	set @msg = replace(@msg, '{n1}', coalesce(cast(@n1 as nvarchar(20)), ''))
	set @msg = replace(@msg, '{n2}', coalesce(cast(@n2 as nvarchar(20)), ''))
	set @msg = replace(@msg, '{n3}', coalesce(cast(@n3 as nvarchar(20)), ''))
	set @msg = replace(@msg, '{n4}', coalesce(cast(@n4 as nvarchar(20)), ''))
	set @msg = replace(@msg, '{n5}', coalesce(cast(@n5 as nvarchar(20)), ''))
	set @msg = replace(@msg, '{n6}', coalesce(cast(@n6 as nvarchar(20)), ''))
	set @msg = replace(@msg, '{n7}', coalesce(cast(@n7 as nvarchar(20)), ''))
	set @msg = replace(@msg, '{s0}', coalesce(@s0, ''))
	set @msg = replace(@msg, '{s1}', coalesce(@s1, ''))
	set @msg = replace(@msg, '{s2}', coalesce(@s2, ''))
	set @msg = replace(@msg, '{s3}', coalesce(@s3, ''))
	set @msg = replace(@msg, '{d0}', coalesce(cast(@d0 as nvarchar(50)), ''))
	set @msg = replace(@msg, '{d1}', coalesce(cast(@d1 as nvarchar(50)), ''))
	set @msg = replace(@msg, '{d2}', coalesce(cast(@d2 as nvarchar(50)), ''))
	set @msg = replace(@msg, '{d3}', coalesce(cast(@d3 as nvarchar(50)), ''))
	set @msg = replace(@msg, '{b0}', case when @b0=1 then 'true' when @b0=0 then 'false' else 'null' end)
	set @msg = replace(@msg, '{b1}', case when @b1=1 then 'true' when @b1=0 then 'false' else 'null' end)
	set @msg = replace(@msg, '{b2}', case when @b2=1 then 'true' when @b2=0 then 'false' else 'null' end)
	set @msg = replace(@msg, '{b3}', case when @b3=1 then 'true' when @b3=0 then 'false' else 'null' end)
	set @msg = replace(@msg, '{m0}', coalesce('$'+cast(@m0 as nvarchar(50)), 'null'))
	set @msg = replace(@msg, '{m1}', coalesce('$'+cast(@m1 as nvarchar(50)), 'null'))
	set @msg = replace(@msg, '{m2}', coalesce('$'+cast(@m2 as nvarchar(50)), 'null'))
	set @msg = replace(@msg, '{m3}', coalesce('$'+cast(@m3 as nvarchar(50)), 'null'))

	if (@startedAt is not null)
	begin
	
		declare @ms int = datediff(millisecond, @startedAt, getdate())
		set @msg = replace(@msg, '{TD}', [dbo].[FormatMillisecondsAsTimespan](@ms));
	 
	end
	
	set @msg = '['+convert(nvarchar(100), getdate(), 126)+'] - '+@msg;

	raiserror (@msg, 0,1) with nowait;

end

GO

CREATE proc [db].[PrintSql]
	@sql nvarchar(max),
	@debugMode bit=0
as
begin

	exec db.PrintNow '{s0}', @s0=@sql

	if (@debugMode=1)
	begin

		select *
		from
		(
			select @sql
			for xml path('Val'), type
		) x(Sql)

	end

end

