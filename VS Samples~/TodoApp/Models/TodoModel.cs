using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace TodoApp.Models
{
    public class TodoModel : ReactiveObject
    {
        [Reactive]
        public bool Did { get; set; }

        [Reactive]
        public string Name { get; set; }
    }
}
