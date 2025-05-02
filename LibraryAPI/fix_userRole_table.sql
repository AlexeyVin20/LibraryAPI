-- Проверяем, есть ли лишнее поле UserId1
DO $$ 
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns 
               WHERE table_name = 'UserRole' AND column_name = 'UserId1') THEN

        -- Убираем внешний ключ, если он есть
        IF EXISTS (
            SELECT 1 FROM information_schema.table_constraints 
            WHERE constraint_name = 'FK_UserRole_Users_UserId1' 
            AND constraint_type = 'FOREIGN KEY'
        ) THEN
            ALTER TABLE "UserRole" DROP CONSTRAINT "FK_UserRole_Users_UserId1";
        END IF;

        -- Удаляем лишнее поле
        ALTER TABLE "UserRole" DROP COLUMN "UserId1";
    END IF;
END $$;

-- Проверяем, есть ли первичный ключ на таблице
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'PK_UserRole' 
        AND constraint_type = 'PRIMARY KEY'
    ) THEN
        -- Если нет, создаем составной первичный ключ
        ALTER TABLE "UserRole" ADD CONSTRAINT "PK_UserRole" PRIMARY KEY ("UserId", "RoleId");
    END IF;
END $$;

-- Проверяем и добавляем внешние ключи, если их нет
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_UserRole_Users_UserId' 
        AND constraint_type = 'FOREIGN KEY'
    ) THEN
        ALTER TABLE "UserRole" ADD CONSTRAINT "FK_UserRole_Users_UserId" 
            FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_UserRole_Roles_RoleId' 
        AND constraint_type = 'FOREIGN KEY'
    ) THEN
        ALTER TABLE "UserRole" ADD CONSTRAINT "FK_UserRole_Roles_RoleId" 
            FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id") ON DELETE CASCADE;
    END IF;
END $$; 