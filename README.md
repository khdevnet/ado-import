# import using bash
```bash
    chmod 777 ./AdoImportTestCases
    ./AdoImportTestCases \
      --organizationUrl "https://dev.azure.com/vswebdeveloper" \
      --patToken "344urpefnuf4skfobpu3fejhlumm7mvo373pxqmwhbbdxabjq" \
      --projectName  "test" \
      --testPlanId  5 \
      --testSuitePath  "auto\\api1" \
      --importFilePath  "testcases.json" 
```