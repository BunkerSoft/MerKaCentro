# Skill: run-tests

<skill-name>
run-tests
</skill-name>

<skill-description>
Ejecuta las pruebas unitarias del proyecto y verifica que el coverage sea >= 80%.
Bloquea el flujo si no se cumple el umbral de cobertura.
</skill-description>

<command-name>
run-tests
</command-name>

<prompt>
Ejecuta las pruebas unitarias del proyecto MiniMarket y verifica la cobertura de código.

## Pasos:

1. **Ejecutar pruebas con cobertura:**
```bash
export PATH="$HOME/.dotnet:$PATH"
cd /home/satanas/Documentos/minimarket
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

2. **Analizar resultados de cobertura:**
- Buscar el archivo coverage.cobertura.xml más reciente en TestResults
- Parsear el archivo para obtener el porcentaje de cobertura de líneas

3. **Generar reporte:**
```markdown
# Test Results Report

## Resumen
- Total de pruebas: X
- Pasaron: X
- Fallaron: X
- Omitidas: X

## Cobertura de Código
- Cobertura de líneas: X%
- Cobertura de ramas: X%
- Umbral requerido: 80%
- Estado: ✅ APROBADO / ❌ RECHAZADO

## Proyectos de Test
| Proyecto | Tests | Pasaron | Fallaron | Cobertura |
|----------|-------|---------|----------|-----------|
| Domain.Tests | X | X | X | X% |
| Application.Tests | X | X | X | X% |
| Infrastructure.Tests | X | X | X | X% |

## Pruebas Fallidas (si las hay)
[Detalle de cada prueba fallida con mensaje de error]

## Áreas con Baja Cobertura
[Listar clases/métodos con cobertura menor al 80%]
```

4. **Verificar umbral:**
- Si la cobertura es >= 80%: Indicar que el código PASA la verificación
- Si la cobertura es < 80%: Indicar que el código NO PASA y listar las áreas que necesitan más tests

5. **Limpiar resultados temporales** si es necesario.
</prompt>

<user_invocable>true</user_invocable>
