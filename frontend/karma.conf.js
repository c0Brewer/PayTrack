coverageReporter: {
  dir: require("path").join(__dirname, "./coverage"),
  reporters: [{ type: "html" }, { type: "text-summary" }],
  check: {
    global: {
      statements: 80,
      branches: 70,
      functions: 80,
      lines: 80
    }
  }
}
