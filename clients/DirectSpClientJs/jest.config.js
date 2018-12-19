
module.exports = {
  roots: ['./test'],
  preset: 'ts-jest',
  testEnvironment: 'node',
  globals: {
    'ts-jest': {
    },
    "tsConfig": {
      "module": "amd"
    }
  }
};
