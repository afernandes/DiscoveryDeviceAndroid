# Distribuição e publicação

## Gerar os arquivos

Na raiz do projeto, execute:

```powershell
.\scripts\Empacotar.ps1
```

O script compila, executa os testes, reúne os avisos de terceiros, recompila o VSIX e valida os arquivos de metadados dentro do pacote. Gera uma pasta versionada sob `dist`, um ZIP para os desenvolvedores e outro ZIP com os materiais da publicação. Não envia arquivos ao Marketplace.

O arquivo `.vsix` é o instalador e também o arquivo aceito no upload do Visual Studio Marketplace. O `.zip` é uma conveniência para compartilhar o VSIX com a documentação. Não envie o ZIP no campo de upload do VSIX.

## Dados desta versão

- Nome: Android via Wi-Fi.
- Versão: 1.0.1.0, Preview.
- Publisher no pacote e manifesto: afernandes, sujeito à confirmação do titular.
- Nome interno sugerido para a loja: AndroidWifi.
- Identificador estável do VSIX: DiscoveryDeviceAndroid.362a398c-e451-4040-8af7-644682ce12ea.
- Plataforma declarada: Windows x64; Visual Studio 17.14 até antes de 19.0.

Confirme o ID do publisher da sua conta antes do primeiro upload. O campo Publisher do VSIX e o ID da conta de publicação são informações relacionadas, mas criar o arquivo não cria nem comprova a posse de uma conta no Marketplace. A licença MIT foi escolhida pelo titular e está incluída no VSIX e nos ZIPs. Os avisos e as licenças das dependências são distribuídos separadamente.

## Publicar pelo navegador

1. Acesse [Gerenciar publicações no Marketplace](https://marketplace.visualstudio.com/manage), entre na sua conta Microsoft e selecione ou crie seu publisher.
2. Selecione **New extension → Visual Studio**.
3. Envie `AndroidWifi-1.0.1-preview.vsix`.
4. Revise nome, versão, publisher, compatibilidade e o nome interno `AndroidWifi`.
5. Use `marketplace/overview.md` como descrição detalhada. O logo já está incluído no VSIX; a pasta de materiais também contém os PNGs.
6. Defina categoria, preço e os demais dados solicitados pelo portal. A proposta dos arquivos é gratuita e classificada como Preview.
7. Acrescente uma captura revisada da interface, se desejar. Não reutilize uma captura que exponha dados pessoais ou de outros projetos.
8. Salve o upload e confira a validação e a página de prévia antes de usar **Make Public**.

O manifesto `publishmanifest.json` foi preparado com `private: true` para a primeira submissão por CLI permanecer privada. No fluxo pelo navegador, confira a visibilidade no próprio portal. Salvar o upload não é o mesmo que tornar a extensão pública.

## Publicação por linha de comando

O SDK oferece `VsixPublisher.exe`. Após configurar sua autenticação na ferramenta, um exemplo é:

```powershell
& 'CAMINHO_DO_VSIXPUBLISHER\VsixPublisher.exe' publish `
    -payload 'CAMINHO_DO_VSIX\AndroidWifi-1.0.1-preview.vsix' `
    -publishManifest 'CAMINHO_DOS_MATERIAIS\marketplace\publishmanifest.json'
```

Os caminhos acima são instruções a adaptar ao ambiente. Prefira o portal para a primeira publicação. Não compartilhe tokens e não os grave no repositório. O pacote não inclui credenciais nem executa publicação automaticamente.

## Atualizações

Mantenha o ID do VSIX e o nome interno da loja. Aumente `Version`, `AssemblyVersion` e `FileVersion` no projeto da extensão, atualize as notas e gere novo pacote. No Marketplace, edite a extensão existente e envie o novo VSIX. Para uma atualização automática, o usuário precisa instalar a mesma identidade de extensão e manter as atualizações habilitadas no IDE.

## Evidência e limites

Os arquivos locais estão preparados para revisão e submissão. A aceitação pelo Marketplace, a instalação limpa em outras máquinas e o pareamento físico não são comprovados pela geração do VSIX ou pelos testes automatizados.

Referências oficiais:

- [Publicar uma extensão do Visual Studio](https://learn.microsoft.com/visualstudio/extensibility/walkthrough-publishing-a-visual-studio-extension).
- [Publicar pela linha de comando](https://learn.microsoft.com/visualstudio/extensibility/walkthrough-publishing-a-visual-studio-extension-via-command-line).
- [Metadados de VisualStudio.Extensibility](https://learn.microsoft.com/dotnet/api/microsoft.visualstudio.extensibility.extensionmetadata?view=vs-extensibility).
