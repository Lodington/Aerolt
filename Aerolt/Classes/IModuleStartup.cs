namespace Aerolt.Classes
{
    public interface IModuleStartup
    {
        public void ModuleStart();

        public void ModuleEnd()
        {
        } // is this even going to work?
    }
}