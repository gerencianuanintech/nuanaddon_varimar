# Cotizacion - AddOn Hoja de Almacenamiento

**Cliente:** Vadimar  
**Proyecto:** Mejoras al AddOn Hoja de Almacenamiento SAP Business One  
**Fecha:** 26/05/2026

## Alcance

Se contempla el desarrollo de mejoras sobre el AddOn Hoja de Almacenamiento, incluyendo ajustes de formulario, carga de archivo Excel, relacion con transferencia SAP, trazabilidad de estado, mensajes de confirmacion, documentacion de validaciones y pruebas funcionales.

## Detalle de Actividades

| # | Requerimiento | Alcance incluido | Horas |
|---|---|---:|
| 1 | Maximizar pantalla | Permitir maximizar la ventana del formulario Hoja de Almacenamiento y validar su comportamiento visual. | 3 |
| 2 | Indicador de carga de archivo `.xlsx` | Mostrar indicador/barra de progreso SAP durante la carga del archivo Excel y bloquear acciones del usuario mientras se procesa la informacion. | 1 |
| 3 | Campo `DocEntry` de transferencia | Agregar campo para guardar el `DocEntry` de la transferencia, manteniendo el numero de transferencia actual, y habilitar navegacion con flecha amarilla al documento SAP. | 3 |
| 4 | Posicion del boton Cancelar | Ajustar la posicion del boton Cancelar segun el estandar visual esperado para los formularios del cliente. | 0.5 |
| 5 | Mensajes de confirmacion | Agregar mensajes de confirmacion al crear la hoja y al realizar la transferencia. | 0.5 |
| 6 | Estado de transferencia | Agregar campo Estado con valores Abierto, Cerrado y Cancelado. Abierto cuando solo se crea la hoja, Cerrado cuando se realiza la transferencia y Cancelado cuando la transferencia relacionada este cancelada. | 3 |
| 7 | Cambio de descripcion de columna | Cambiar la descripcion visible de la columna Embalaje por Caja. | 0.5 |
| 8 | Lista de mensajes de validacion | Elaborar la lista de mensajes de validacion contemplados en el AddOn, indicando significado y accion sugerida. | 0.5 |
| 9 | Pruebas funcionales y ajustes | Validacion en SAP Business One de carga de archivo, creacion de hoja, transferencia, relacion por flecha amarilla, estados y ajustes menores. | 2 - 3 |
| 10 | Flechas amarillas en detalle | Habilitar navegacion con flecha amarilla en columnas de Item, Lote y Ubicaciones para acceder a los documentos o maestros relacionados en SAP Business One. | 2 |

## Resumen de Horas

| Concepto | Horas |
|---|---:|
| Desarrollo y documentacion | 14 |
| Pruebas funcionales y ajustes | 2 - 3 |
| **Total estimado** | **16 - 17** |

## Lista de Mensajes de Validacion

Como parte del alcance se entregara una matriz funcional con los mensajes que puede mostrar el AddOn durante la carga del archivo, validacion de lineas, creacion de hoja de almacenamiento y generacion de transferencia.

| Mensaje / validacion | Significado |
|---|---|
| La bodega de origen y destino son requeridas | El usuario debe seleccionar ambas bodegas antes de cargar informacion. |
| Fecha vencimiento no valida | El valor de fecha del Excel no tiene un formato reconocido. |
| Valor en Cant. Total no valido | La cantidad total del Excel no es numerica o no se pudo interpretar. |
| Valor en Cantidad 1 a 15 no valido | Una de las columnas de cantidades por ubicacion contiene un valor invalido. |
| Valor en embalaje no valido | El valor de embalaje/caja no es numerico o no cumple la validacion. |
| El embalaje debe ser mayor que 0 | Para el item validado, la cantidad de cajas debe ser mayor a cero. |
| La pieza debe ser mayor que 0 para este item | El item requiere piezas y el valor ingresado es cero o vacio. |
| La pieza debe ser 0 para este item | El item no requiere piezas, pero se cargo un valor mayor a cero. |
| Suma de cantidades por ubicacion distinta a cantidad total | La suma de cantidades por ubicacion no coincide con la cantidad total de la linea. |
| Cantidad no disponible para el lote | La cantidad solicitada supera el stock disponible del lote en la bodega origen. |
| Debe cargar al menos una linea antes de crear la transferencia | No existen lineas cargadas en la matriz para transferir. |
| No se puede crear la transferencia. Revise las observaciones de las lineas | Una o mas lineas tienen observaciones distintas de OK. |
| Transferencia existente con Numero | La hoja ya tiene una transferencia relacionada. |
| DIAPI Error | SAP Business One rechazo la creacion de la transferencia. El detalle depende del error devuelto por SAP. |

## Consideraciones

- El ajuste de posicion del boton Cancelar queda sujeto a confirmacion visual del estandar esperado por Vadimar.
- La lista de mensajes contemplara las validaciones actualmente existentes en el AddOn y las que se ajusten como parte de este requerimiento.
- No se incluye desarrollo de nuevos reportes, cambios adicionales al proceso de negocio ni soporte posterior fuera del alcance indicado.
- Las horas son estimadas y pueden variar si durante las pruebas se detectan validaciones adicionales requeridas por SAP Business One o por la base de datos del cliente.

## Tiempo Estimado

El tiempo estimado de ejecucion es de **16 a 17 horas**, sujeto a disponibilidad de ambiente SAP Business One para pruebas.
