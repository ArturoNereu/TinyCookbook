using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Input;
using Unity.Tiny.Text;
#if UNITY_WEBGL
using Unity.Tiny.HTML;
#endif

namespace BrowserInterop
{
    public struct UserName : IComponentData { }
    public struct UserEmail : IComponentData { }
    public struct UserId : IComponentData { }
    public struct LoadingMessage : IComponentData { }

#if UNITY_WEBGL
    public class RandomUserDataSystem : ComponentSystem
    {
        private static readonly NativeString512 UserNameMessageType = new NativeString512("FetchRandomUserProfileMessage-UserName");
        private static readonly NativeString512 EmailMessageType = new NativeString512("FetchRandomUserProfileMessage-Email");
        private static readonly NativeString512 UserIdMessageType = new NativeString512("FetchRandomUserProfileMessage-UserId");

        [DllImport("__Internal")]
        private static extern void FetchRandomUserProfile();

        protected override void OnUpdate()
        {
            var inputSystem = World.GetExistingSystem<InputSystem>();

            bool shouldCallJsFunction = inputSystem.IsTouchSupported() ? inputSystem.TouchCount() > 0 && inputSystem.GetTouch(0).phase == TouchState.Ended
                                                                       : inputSystem.GetKeyUp(KeyCode.Space) || inputSystem.GetMouseButtonUp(0);

            if (shouldCallJsFunction)
            {
                SetLoadingMessageVisibility(true);
                FetchRandomUserProfile();
            }

            Entities.ForEach((Entity e, ref NativeMessage nativeMessage, DynamicBuffer<NativeMessageByte> bytesBuffer) =>
            {
                SetLoadingMessageVisibility(false);

                if (nativeMessage.message.CompareTo(UserNameMessageType) == 0)
                    SetText<UserName>(bytesBuffer);
                else if(nativeMessage.message.CompareTo(EmailMessageType) == 0)
                    SetText<UserEmail>(bytesBuffer);
                else if (nativeMessage.message.CompareTo(UserIdMessageType) == 0)
                    SetText<UserId>(bytesBuffer);

                PostUpdateCommands.DestroyEntity(e);
            });
        }

        private void SetLoadingMessageVisibility(bool isLoading)
        {
            Entities.WithAll<LoadingMessage>()
                    .ForEach((ref NonUniformScale t) => t.Value = isLoading ? 1.0f : 0.0f);
        }

        private void SetText<T>(DynamicBuffer<NativeMessageByte> byteBuffer) where T : IComponentData
        {
            var str = NativeMessageByteBufferToString(byteBuffer);

            Entities
                .WithAll<T>()
                .ForEach((Entity e) => EntityManager.SetBufferFromString<TextString>(e, str));
        }

        private string NativeMessageByteBufferToString(DynamicBuffer<NativeMessageByte> byteBuffer)
        {
            var charArray = new char[byteBuffer.Length];
            for (var i = 0; i < byteBuffer.Length; i++)
            {
                charArray[i] = (char)byteBuffer[i].Value;
            }
            return new string(charArray);
        }
    }
#endif
}
