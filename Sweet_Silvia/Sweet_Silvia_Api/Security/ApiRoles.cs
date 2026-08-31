namespace Sweet_Silvia_Api.Security;

public static class ApiRoles
{
    public const string Customer = "Cliente";
    public const string Administrator = "Administrador";
    public const string Assistant = "Asistente";
    public const string AnyAuthenticated = Customer + "," + Administrator + "," + Assistant;
    public const string AdministratorOrAssistant = Administrator + "," + Assistant;
}
