pipeline {
    agent any

    options {
        disableConcurrentBuilds()
        timeout(time: 20, unit: 'MINUTES')
    }

    stages {
        stage('Check database') {
            steps {
                sh '''
                    test "$(docker inspect -f '{{.State.Running}}' todoappdb)" = "true"
                    test "$(docker inspect -f '{{.State.Health.Status}}' todoappdb)" = "healthy"
                '''
            }
        }

        stage('Build') {
            steps {
                sh '''
                    docker build -t todoapp-ci:${BUILD_NUMBER} ./TodoApp
                    docker build -f Dockerfile.tests -t todoapp-tests:${BUILD_NUMBER} .
                '''
            }
        }

        stage('Test') {
            steps {
                sh '''
                    docker run --rm \
                      --network container:todoappdb \
                      todoapp-tests:${BUILD_NUMBER}
                '''
            }
        }

        stage('Deploy') {
            steps {
                sh '''
                    set +x
                    set -eu

                    NETWORK=$(docker inspect todoappdb \
                      --format '{{range $name, $settings := .NetworkSettings.Networks}}{{println $name}}{{end}}' \
                      | head -n 1)

                    DB_PASSWORD=$(docker exec todoappdb printenv MARIADB_PASSWORD)

                    export ConnectionStrings__TodoDb="Server=todoappdb;Port=3306;Database=todo_db;User=todo_usr;Password=${DB_PASSWORD};"

                    docker rm -f todoapp

                    docker run -d \
                      --name todoapp \
                      --restart unless-stopped \
                      --network "$NETWORK" \
                      -p 8081:8080 \
                      -e ConnectionStrings__TodoDb \
                      -e ASPNETCORE_ENVIRONMENT=Development \
                      todoapp-ci:${BUILD_NUMBER}
                '''
            }
        }

        stage('Check website') {
            steps {
                sh '''
                    docker run --rm \
                      --network container:todoapp \
                      curlimages/curl:latest \
                      --fail --silent --show-error \
                      --retry 15 --retry-connrefused --retry-delay 2 \
                      http://localhost:8080/
                '''
            }
        }
    }

    post {
        failure {
            echo 'Pipeline mislukt. Bekijk de eerste fout in de console-output.'
        }
        success {
            echo 'Build, tests, deployment en websitecontrole geslaagd.'
        }
    }
}
