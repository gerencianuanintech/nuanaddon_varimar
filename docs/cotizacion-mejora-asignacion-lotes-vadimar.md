# Cotizacion - Mejora Asignacion de Lotes

**Cliente:** Vadimar  
**Proceso:** Asignacion automatica de lotes en SAP Business One  
**Fecha:** 26/05/2026

## Alcance

Se contempla el ajuste de la automatizacion actual de asignacion de lotes, manteniendo las reglas existentes por cliente y agregando nuevos criterios de priorizacion para la seleccion de lotes.

## Reglas Actuales Contempladas

| Cliente | Regla actual |
|---|---|
| Hip. Tottus | Selecciona lotes con vencimiento mayor a fecha de entrega + 5 meses. |
| Cencosud | Selecciona lotes con vencimiento mayor a fecha de entrega + 4 meses. |
| Sup. Peruanos | Aplica regla especial considerando ultima entrega del articulo y vida util del lote. |

Actualmente la automatizacion valida la disponibilidad de cantidad y muestra mensaje cuando no existen lotes disponibles que cumplan las reglas del cliente.

## Nueva Solicitud

Se requiere ajustar el boton de seleccion de lotes para que aplique los criterios en el siguiente orden:

1. Regla del cliente.
2. Fecha de vencimiento.
3. Menor cantidad disponible.
4. Orden alfabetico del identificador final del lote.

Ejemplo: en el lote `C356 A2`, el orden alfabetico debe considerar el valor final `A2`.

Adicionalmente, se requiere agregar un campo o parametro para adicionar **7 dias** a la fecha utilizada en las reglas de seleccion de lotes.

## Detalle de Actividades

| # | Requerimiento | Alcance incluido | Horas |
|---|---|---:|
| 1 | Analisis de reglas actuales | Revisar funciones actuales y confirmar comportamiento base por cliente. | 0.5 |
| 2 | Ajuste de orden de seleccion | Modificar la logica para aplicar orden por regla de cliente, fecha de vencimiento, menor cantidad disponible y orden alfabetico del lote. | 3 |
| 3 | Lectura del identificador final del lote | Considerar el valor final del lote, por ejemplo `A2` en `C356 A2`, como criterio de ordenamiento. | 1.5 |
| 4 | Campo/parametro de dias adicionales | Agregar campo o parametro para adicionar 7 dias a la fecha usada en la seleccion de lotes. | 0.5 |
| 5 | Ajuste por cliente | Validar que la nueva logica aplique correctamente para Tottus, Cencosud y Sup. Peruanos sin romper reglas actuales. | 2 |
| 6 | Pruebas funcionales | Pruebas con distintos lotes, fechas de vencimiento, cantidades disponibles y codigos alfabeticos. | 2 |

## Resumen de Horas

| Concepto | Horas |
|---|---:|
| Analisis y desarrollo | 7.5 |
| Pruebas funcionales y ajustes | 2 |
| **Total estimado** | **9.5** |

## Texto Sugerido

Se estima un esfuerzo de **9.5 horas** para ajustar la automatizacion de asignacion de lotes, manteniendo las reglas actuales por cliente y agregando nuevos criterios de priorizacion: regla del cliente, fecha de vencimiento, menor cantidad disponible y orden alfabetico del identificador final del lote. Adicionalmente, se agregara un campo o parametro para considerar 7 dias adicionales en la fecha utilizada por las reglas de seleccion.

## Consideraciones

- El nuevo orden se aplicara unicamente sobre los lotes que cumplan la regla del cliente correspondiente.
- Se tomara como referencia que el identificador alfabetico del lote se encuentra al final del numero de lote, por ejemplo `A2` en `C356 A2`.
- Se requiere confirmar si el campo de dias adicionales sera visible para el usuario, un parametro de configuracion o un valor fijo dentro de la automatizacion.
- La automatizacion actual depende de los identificadores visuales de SAP Business One; cambios en pantalla, columnas visibles o configuracion del formulario pueden requerir ajustes adicionales.
