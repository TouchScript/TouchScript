1. Создаем новый Behaviour: Breaker.
2. Добавляем на Prefabs/Cube TapGesture и Breaker.
3. В Breaker.Start() подписываемся на событие StateChanged у TapGesture. 

GetComponent<TapGesture>().StateChanged += HandleStateChanged;

4. Пишем обработчик события
void HandleStateChanged (object sender, TouchScript.Events.GestureStateChangeEventArgs e) {
  
}

Нам нужно условие (e.State == Gesture.GestureState.Recognized), потому что этот обработчик срабатывает на любое изменение состояния жеста (в том числе и на Gesture.GestureState.Failed).
5. Проверяем, что на Prefabs/Cube есть коллайдер (он используется для того, чтобы определить, что тачи попали на объект).
6. Запускаем, должно работать.
7. При старте автоматически создается TouchScript объект с TouchManager компонентом и на главную камеру вешается Camera Layer. Если хочется контролировать их настройки в более сложных сетапах, нужно самому создать TouchManager, повесить на него нужные источники тачей TuioInput, iOSInput и так далее и повесить CameraLayer на главную камеру.
8. Чтобы видеть дебаг тачи, нужно добавить в сцену префаб Debug Camera.