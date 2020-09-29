using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TodoApp.Models;

namespace TodoApp.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        [Reactive]
        public string Input { get; set; }

        public ObservableCollection<TodoModel> Todos { get; set; }

        public MainViewModel()
        {
            Todos = new ObservableCollection<TodoModel>();
            Input = string.Empty;

            var nomEmpty = this.WhenAnyValue(m => m.Input,
                i => !string.IsNullOrEmpty(i));
            AddCommand = ReactiveCommand.Create(AddTodo, nomEmpty);

        }

        public ReactiveCommand<Unit, Unit> AddCommand { get; set; }


        public void AddTodo()
        {
            Todos.Add(new TodoModel
            {
                Name = Input
            });
            Input = string.Empty;
        }
    }
}
