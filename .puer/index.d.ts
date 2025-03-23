declare namespace CS {
    namespace EFramework.Utility {
        namespace XObject {
            /**
             * 函数this实例绑定器
             */
            export function This(): (target: any, propertyKey: string) => void

            /**
             * 对象实例哈希
             * @param obj 对象实例
             */
            export function HashCode(obj: any): string

            /**
             * 获取类型
             * @param type 类型
             */
            export function TypeOf(type: any): CS.System.Type
        }
    }
}