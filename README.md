# Construction Site Imports

Complemento para Timberborn que faz obras atendidas pelo **Construction Site Hauler** solicitarem materiais pela travessia distrital, sem exigir um armazém para cada material. Por exemplo, uma obra que precisa de 20 papéis pode recebê-los de outro distrito.

Requer Timberborn **1.1.2.8**, Construction Site Hauler **11.0.2** e suas dependências: Moddable Timberborn, TimberUI e Harmony. As versões usadas na preparação estão em [REVISAO.txt](REVISAO.txt).

Ative os dois mods no jogo, deixe o material em **Import If Needed** no destino e mantenha a travessia com trabalhadores e acesso aos distritos. A origem precisa permitir a exportação e ter estoque disponível. Os transportadores locais levam o material da travessia até a obra. As regras e a instalação manual estão em [LEIA-ME.txt](LEIA-ME.txt).

Para compilar no Windows, siga [COMO-COMPILAR.txt](COMO-COMPILAR.txt): instale o SDK .NET 10 local, prepare as referências da sua instalação e execute `BuildSupport/Build-ConstructionSiteImports.ps1 -SteamApps '<sua biblioteca Steam>\steamapps'`. O resultado fica em `staging/ConstructionSiteImports/version-1.1`. O script não instala o complemento no jogo.

O repositório inclui o código do complemento, os arquivos compartilhados necessários, o preparador de referências e a verificação em `ImportTests`. As bibliotecas do jogo e dos mods dependentes precisam estar instaladas localmente; não estão incluídas aqui. Os caminhos para a Steam são configuráveis e devem apontar para a sua instalação.

Depois de compilar, execute no PowerShell, a partir da raiz do repositório:

```powershell
& .\dotnet\dotnet.exe run --project .\ImportTests\ImportTests.csproj -c Release -- '<sua biblioteca Steam>\steamapps'
```

A preparação original compilou sem avisos nem erros e passou em **43 verificações** de cálculos e integração com as bibliotecas instaladas. O fluxo completo de transporte **ainda precisa ser testado dentro do jogo**. Não houve publicação na Steam.

Baseado no trabalho de Luke Vo em [datvm/TimberbornMods](https://github.com/datvm/TimberbornMods), commit `dab484c3fd194d182170e837046f7fc5ac03f9f9`. O [Construction Site Hauler original](https://steamcommunity.com/sharedfiles/filedetails/?id=3779332547) continua necessário. A [licença MIT](LICENSE) e os avisos originais foram preservados.
