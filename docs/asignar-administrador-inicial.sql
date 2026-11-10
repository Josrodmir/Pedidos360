-- Ejecute en Pedidos360DB después de registrar el primer usuario.
-- Reemplace el correo por el del administrador inicial.
DECLARE @Correo nvarchar(256) = N'correo-del-administrador@ejemplo.com';
DECLARE @UsuarioId nvarchar(450) = (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = UPPER(@Correo));
DECLARE @RolId nvarchar(450) = (SELECT Id FROM AspNetRoles WHERE NormalizedName = N'ADMINISTRADOR');

IF @UsuarioId IS NULL THROW 50001, 'No existe un usuario con ese correo.', 1;
IF @RolId IS NULL THROW 50002, 'El rol Administrador aún no existe. Inicie la aplicación una vez y vuelva a intentar.', 1;

IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UsuarioId AND RoleId = @RolId)
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UsuarioId, @RolId);
